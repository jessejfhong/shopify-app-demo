use actix_web::{web::Query, HttpResponse, Responder};
use serde::Deserialize;

#[derive(Deserialize)]
pub struct AuthResultInfo {
    host: String,
    shop: String,
    state: String,
    code: String,
}

pub async fn auth_result(info: Query<AuthResultInfo>) -> impl Responder {
    HttpResponse::Ok().content_type("text/html").body(format!(
        "host={}&shop={}&state={}&code={}",
        info.host, info.shop, info.state, info.code
    ))
}
