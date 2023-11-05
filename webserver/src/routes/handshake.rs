use actix_web::{web::Query, HttpResponse, Responder};
use serde::Deserialize;

#[derive(Deserialize)]
pub struct HandshakeInfo {
    host: String,
    shop: String,
}

pub async fn handshake(info: Query<HandshakeInfo>) -> impl Responder {
    HttpResponse::Ok()
        .content_type("text/html")
        .body(format!("host={}&shop={}", info.host, info.shop))
}
