use actix_web::{HttpResponse, Responder};

pub async fn health_check() -> impl Responder {
    HttpResponse::Ok()
        .content_type("text/html")
        .body("Hey there!")
}
