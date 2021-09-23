module App

open Elmish
open Feliz
open Feliz.Bulma
open Fable.Core.JsInterop

importSideEffects "./../style/style.scss"

[<RequireQualifiedAccess>]
type Page =
    | Home
    | NotFound

type Msg =
    | SetRoute of Router.Route option


type Model =
    {
        CurrentRoute : Router.Route option
        ActivePage : Page
    }


let private setRoute (optRoute : Router.Route option) (model : Model) =
    let model = { model with CurrentRoute = optRoute }

    match optRoute with
    | None ->
        { model with
            ActivePage = Page.NotFound
        }
        , Cmd.none

    | Some route ->
        match route with
        | Router.Route.Home ->
            { model with
                ActivePage = Page.Home
            }
            , Cmd.none

        | Router.Route.NotFound ->
            { model with
                ActivePage = Page.NotFound
            }
            , Cmd.none


let private update (msg : Msg) (model : Model) =
    match msg with
    | SetRoute optRoute ->
        setRoute optRoute model


let private init (location : Router.Route option) =
    setRoute
        location
        {
            ActivePage = Page.Home
            CurrentRoute = None
        }


open Elmish.React
open Elmish.UrlParser
open Elmish.Navigation


let private view (model : Model) (dispatch : Dispatch<Msg>) =
    Bulma.columns [
        Bulma.column [
            column.isOffset2
            column.is3

            prop.children [
                Bulma.box [
                    color.hasBackgroundPrimary
                    text.hasTextCentered

                    prop.children [
                        Bulma.button.a [
                            prop.text "Go to Visio page"
                        ]
                    ]
                ]
            ]
        ]

        Bulma.column [
            column.isOffset2
            column.is3

            prop.children [
                Bulma.box [
                    color.hasBackgroundPrimary
                    text.hasTextCentered

                    prop.children [
                        Bulma.button.a [
                            prop.text "Go to form page"
                        ]
                    ]
                ]
            ]
        ]
    ]

#if DEBUG
open Elmish.HMR
#endif

Program.mkProgram init update view
|> Program.toNavigable (parseHash Router.routeParser) setRoute
|> Viewable.Program.withViewable
|> Program.withReactSynchronous "root"
|> Program.run
