use actix_web::dev::Service;
use actix_web::web::{get, to, Data};
use actix_web::{middleware::Logger, web, App, HttpResponse, HttpServer, Responder};
use futures_util::future::FutureExt;
//use webserver::middleware::VerifyShopifyRequest;
use shopifyrs;
use webserver::routes::{auth_result, handshake, health_check};
use webserver::AppState;

async fn not_found() -> impl Responder {
    HttpResponse::NotFound().body("Not Found!")
}

#[tokio::main]
async fn main() -> std::io::Result<()> {
    let app_state = Data::new(AppState {
        client_secret: String::from("my client secret"),
    });

    HttpServer::new(move || {
        let shopify = web::scope("/shopify")
            .wrap_fn(|req, srv| {
                // TODO: verify the hmac from the request query string
                // see https://shopify.dev/docs/apps/auth/oauth/getting-started#step-2-verify-the-installation-request

                println!("Hello from looper. query string: {}", req.query_string());

                // get hmac from req parameter
                let hmac = "hmacstring";

                if let Some(app_state) = req.app_data::<Data<AppState>>() {
                    println!("app state: {}", app_state.client_secret);

                    if shopifyrs::verify_shopify_request(hmac, &app_state.client_secret) {
                        // continue
                    } else {
                        // circuit
                    }
                }

                srv.call(req).map(|res| {
                    println!("Hello from response.");
                    res
                })
            })
            //.wrap(VerifyShopifyRequest)
            .route("/handshake", get().to(handshake))
            .route("/authresult", get().to(auth_result));

        App::new()
            .app_data(app_state.clone())
            .wrap(Logger::default())
            .wrap(Logger::new("%a %{User-Agent}i"))
            .route("/health_check", get().to(health_check))
            .service(shopify)
            .default_service(to(not_found))
    })
    .bind(("127.0.0.1", 8080))?
    .run()
    .await
}
