CREATE TABLE installation(
    id SERIAL NOT NULL,
    shopify_shop_id BIGINT,
    shopify_shop_domain VARCHAR(256) NOT NULL UNIQUE,
    shopify_access_token VARCHAR(256),
    updated_at TIMESTAMPTZ NOT NULL,
    PRIMARY KEY (id)
);
