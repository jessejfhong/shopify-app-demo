port module Main exposing (main)

import Browser exposing (Document)
import Html exposing (button, div, footer, h2, header, main_, p, text)
import Html.Attributes exposing (class)
import Html.Events exposing (onClick)
import Http



{-
   how to send http request in the context of Shopify embeded app
   0. something trigger an action to ask for data from backend api
   1. how some data need to pass to the http get, post function, etc
   2. need to get the session token from shopify app bridge, which is outside of elm app


-}


port requestSessionToken : () -> Cmd msg


port sessionToken : (String -> msg) -> Sub msg


type Msg
    = Sayhello
    | GotHealthCheck (Result Http.Error String)
    | GotSessionToken String
    | ButtonClicked String


type alias Model =
    { title : String
    , origin : String
    , message : String
    }


healthCheck token =
    Http.request
        { method = "GET"
        , headers = [ Http.header "Authorization" ("Bearer " ++ token) ]
        , url = "http://localhost:8080/api/home/healthcheck"
        , body = Http.emptyBody
        , expect = Http.expectString GotHealthCheck
        , timeout = Nothing
        , tracker = Nothing
        }


init : String -> ( Model, Cmd Msg )
init origin =
    ( Model "Shopify App Demo" origin "Hey there!", Cmd.none )


view : Model -> Document Msg
view model =
    let
        headerView =
            header [] [ h2 [ class "text-3xl", class "font-blod", class "underline" ] [ text "Header" ] ]

        mainView =
            main_ []
                [ div [] [ button [ ButtonClicked "data" |> onClick ] [ text "Click" ] ]
                , p [] [ text model.message ]
                ]

        footerView =
            footer [] [ text "footer section" ]
    in
    { title = model.title
    , body =
        [ headerView
        , mainView
        , footerView
        ]
    }


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    case msg of
        Sayhello ->
            ( model, Cmd.none )

        GotHealthCheck result ->
            case result of
                Ok str ->
                    ( { model | message = str }, Cmd.none )

                Err _ ->
                    ( { model | message = "Oops, something gone wroing!" }, Cmd.none )

        GotSessionToken token ->
            -- after getting a session toke, make http reqest
            ( model, healthCheck token )

        ButtonClicked data ->
            -- first step of making http request, get the session token from app bridge
            ( model, requestSessionToken () )


subscriptions : Model -> Sub Msg
subscriptions model =
    Sub.batch
        [ sessionToken GotSessionToken
        ]


main : Program String Model Msg
main =
    Browser.document
        { init = init
        , view = view
        , update = update
        , subscriptions = subscriptions
        }
