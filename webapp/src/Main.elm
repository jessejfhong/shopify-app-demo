module Main exposing (main)

import Browser exposing (Document)
import Html exposing (div, footer, h2, header, main_, text)
import Html.Attributes exposing (class)
import Http


type Msg
    = Sayhello
    | GotHealthCheck (Result Http.Error String)


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
                    ( { model | message = str }, Cmd.none )

                Err _ ->
                    ( { model | message = "Oops, something gone wroing!" }, Cmd.none )


main : Program String Model Msg
main =
    Browser.document
        { init = init
        , view = view
        , update = update
        , subscriptions = \_ -> Sub.none
        }
