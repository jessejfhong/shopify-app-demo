use actix_web::dev::Service;
use actix_web::web::{get, scope, to, Data};
use actix_web::{middleware::Logger, App, HttpResponse, HttpServer, Responder};
//use futures_util::future::Either;
use env_logger::Env;
use futures_util::future::FutureExt;
use webserver::middleware::VerifyShopifyRequest;
//use shopifyrs;
use webserver::routes::{auth_result, handshake, health_check};
use webserver::AppState;

async fn not_found() -> impl Responder {
    HttpResponse::NotFound().body("Not Found!")
}

#[tokio::main]
async fn main() -> std::io::Result<()> {
    env_logger::init_from_env(Env::default().default_filter_or("info"));

    let app_state = Data::new(AppState {
        client_secret: String::from("very strong secert"),
    });

    HttpServer::new(move || {
        let shopify = scope("/shopify")
            .wrap_fn(|req, srv| {
                println!("Hello from req fn2.");

                srv.call(req).map(|res| {
                    println!("Hello from res fn2.");
                    res
                })
            })
            // .wrap_fn(|req, srv| {
            //     // see https://shopify.dev/docs/apps/auth/oauth/getting-started#step-2-verify-the-installation-request
            //     println!("Hello from req fn1.");
            //     let is_authentic = req
            //         .app_data::<Data<AppState>>()
            //         .map(|app_state| {
            //             let url = format!("{}://{}{}", req.connection_info().scheme(), req.connection_info().host(), req.uri().to_string());
            //             shopifyrs::verify_shopify_request(
            //                 &url,
            //                 &app_state.client_secret,
            //             )
            //         })
            //         .unwrap_or(false);
            //     let either = match is_authentic {
            //         true => Either::Right(srv.call(req)),
            //         false => {
            //             Either::Left(req.into_response(HttpResponse::Forbidden().body("Forbidden")))
            //         }
            //     };
            //     Box::pin(async move {
            //         println!("Hello from res fn1.");
            //         match either {
            //             Either::Left(res) => Ok(res),
            //             Either::Right(fut) => fut.await,
            //         }
            //     })
            // })
            .wrap(VerifyShopifyRequest)
            .route("/handshake", get().to(handshake))
            .route("/authresult", get().to(auth_result));

        App::new()
            //.wrap(Logger::default())
            .wrap(Logger::new("%a %{User-Agent}i"))
            .app_data(app_state.clone())
            .route("/health_check", get().to(health_check))
            .service(shopify)
            .default_service(to(not_found))
    })
    .bind(("127.0.0.1", 8080))?
    .run()
    .await
}
