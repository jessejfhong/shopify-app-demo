port module Main exposing (main)

import Browser exposing (Document)
import Html exposing (div, footer, h2, header, main_, text)
import Html.Attributes exposing (class)
import Http


port requestSessionToken : () -> Cmd msg


port sessionToken : (String -> msg) -> Sub msg


type Msg
    = Sayhello
    | GotHealthCheck (Result Http.Error String)
    | GotSessionToken String


type alias Model =
    { title : String
    , origin : String
    , message : String
    }


healthCheck =
    Http.get
        { url = "http://localhost:5173/health_check"
        , expect = Http.expectString GotHealthCheck
        }


init : String -> ( Model, Cmd Msg )
init origin =
    ( Model "Shopify App Demo" origin "Hey there!", healthCheck )


view : Model -> Document Msg
view model =
    let
        headerView =
            header [] [ h2 [ class "text-3xl", class "font-blod", class "underline" ] [ text "Header" ] ]

        mainView =
            main_ [] [ text model.message ]

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
                    ( { model | message = str }, requestSessionToken () )

                Err _ ->
                    ( { model | message = "Oops, something gone wroing!" }, Cmd.none )

        GotSessionToken token ->
            ( { model | message = token }, Cmd.none )


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
