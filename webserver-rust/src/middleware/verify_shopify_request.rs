use crate::AppState;
use actix_web::body::EitherBody;
use actix_web::web::Data;
use actix_web::{
    dev::{forward_ready, Service, ServiceRequest, ServiceResponse, Transform},
    Error, HttpResponse,
};
use futures_util::future::LocalBoxFuture;
use shopifyrs;
use std::future::{ready, Ready};

pub struct VerifyShopifyRequest;

impl<S, B> Transform<S, ServiceRequest> for VerifyShopifyRequest
where
    S: Service<ServiceRequest, Response = ServiceResponse<B>, Error = Error>,
    S::Future: 'static,
    B: 'static,
{
    type Response = ServiceResponse<EitherBody<B>>;
    type Error = Error;
    type InitError = ();
    type Transform = VerifyShopifyRequestMiddleware<S>;
    type Future = Ready<Result<Self::Transform, Self::InitError>>;

    fn new_transform(&self, service: S) -> Self::Future {
        ready(Ok(VerifyShopifyRequestMiddleware { service }))
    }
}

pub struct VerifyShopifyRequestMiddleware<S> {
    service: S,
}

impl<S, B> Service<ServiceRequest> for VerifyShopifyRequestMiddleware<S>
where
    S: Service<ServiceRequest, Response = ServiceResponse<B>, Error = Error>,
    S::Future: 'static,
    B: 'static,
{
    type Response = ServiceResponse<EitherBody<B>>;
    type Error = Error;
    type Future = LocalBoxFuture<'static, Result<Self::Response, Self::Error>>;

    forward_ready!(service);

    fn call(&self, req: ServiceRequest) -> Self::Future {
        let is_authentic = req
            .app_data::<Data<AppState>>()
            .map(|app_state| {
                let url = format!(
                    "{}://{}{}",
                    req.connection_info().scheme(),
                    req.connection_info().host(),
                    req.uri().to_string()
                );
                shopifyrs::verify_shopify_request(&url, &app_state.client_secret)
            })
            .unwrap_or(false);

        if !is_authentic {
            let (request, _pl) = req.into_parts();
            let response = HttpResponse::Forbidden()
                .content_type("text/plain;charset=utf-8")
                .body("Forbidden")
                .map_into_right_body();

            Box::pin(async { Ok(ServiceResponse::new(request, response)) })
        } else {
            let res = self.service.call(req);
            Box::pin(async move { res.await.map(ServiceResponse::map_into_left_body) })
        }
    }
}
