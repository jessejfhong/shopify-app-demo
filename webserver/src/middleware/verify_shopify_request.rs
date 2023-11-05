use crate::AppState;
use actix_web::web;
use actix_web::{
    dev::{forward_ready, Service, ServiceRequest, ServiceResponse, Transform},
    Error,
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
    type Response = ServiceResponse<B>;
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
    type Response = ServiceResponse<B>;
    type Error = Error;
    type Future = LocalBoxFuture<'static, Result<Self::Response, Self::Error>>;

    forward_ready!(service);

    fn call(&self, req: ServiceRequest) -> Self::Future {
        println!("You requested: {}", req.path());

        // get hmac from req parameter
        let hmac = "hmacstring";

        if let Some(app_state) = req.app_data::<web::Data<AppState>>() {
            println!("app state: {}", app_state.client_secret);

            if shopifyrs::verify_shopify_request(hmac, &app_state.client_secret) {
                // continue
            } else {
                // circuit
            }
        }

        let fut = self.service.call(req);

        Box::pin(async move {
            let res = fut.await?;

            println!("Hi from response!");

            Ok(res)
        })
    }
}
