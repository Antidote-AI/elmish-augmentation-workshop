module App

open Elmish
open Feliz
open Feliz.Bulma
open Fable.Core.JsInterop

importSideEffects "./../style/style.scss"

module Form = Page.Form.Component

[<RequireQualifiedAccess>]
type Page =
    | Home
    | NotFound
    | Form of Form.Model

    member this.isHome
        with get () =
            match this with
            | Home ->
                true

            | _ ->
                false

    member this.isForm
        with get () =
            match this with
            | Form _ ->
                true

            | _ ->
                false
type Msg =
    | SetRoute of Router.Route option
    | FormMsg of Form.Msg
    | StartCall

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

        | Router.Route.Form ->
            let (subModel, subCmd) = Form.init ()
            { model with
                ActivePage = Page.Form subModel
            }
            , Cmd.map FormMsg subCmd


let private update (msg : Msg) (model : Model) =
    match msg with
    | SetRoute optRoute ->
        setRoute optRoute model

    | FormMsg subMsg ->
        match model.ActivePage with
        | Page.Form subModel ->
            Form.update subMsg subModel
            |> Tuple.mapFirst Page.Form
            |> Tuple.mapFirst (fun page -> { model with ActivePage = page })
            |> Tuple.mapSecond (Cmd.map FormMsg)

        | _ ->
            model
            , Cmd.none

    | StartCall ->
        model
        , Viewable.connect


let private init (location : Router.Route option) =
    setRoute
        location
        {
            ActivePage = Page.Home
            CurrentRoute = None
        }

let private renderNavbarLink
    (route : Router.Route)
    (isActive : bool)
    (text : string) =

    Bulma.navbarItem.a [
        Router.href route

        if isActive then
            navbarItem.isActive

        prop.text text
    ]

let private navbar (model : Model) =
    Bulma.navbar [
        navbar.isFixedTop
        color.isPrimary

        prop.children [

            Bulma.container [
                Bulma.navbarBrand.div [
                    Bulma.navbarItem.div [
                        Bulma.text.div [
                            size.isSize4
                            prop.text "Demo"
                        ]
                    ]
                ]

                Bulma.navbarMenu [
                    Bulma.navbarStart.div [
                        renderNavbarLink
                            Router.Route.Home
                            model.ActivePage.isHome
                            "Home"

                        renderNavbarLink
                            Router.Route.Form
                            model.ActivePage.isForm
                            "Form"
                    ]
                ]
            ]
        ]
    ]

open Fable.FontAwesome

let private view (model : Model) (dispatch : Dispatch<Msg>) =
    let pageContent =
        match model.ActivePage with
        | Page.Home ->
            Bulma.section [

                Html.text "Home page"

                Html.br []
                Html.br []

                Bulma.button.button [
                    prop.onClick (fun _ ->
                        dispatch StartCall
                    )

                    prop.text "Start call"
                ]
            ]

        | Page.NotFound ->
            Html.text "Not found"

        | Page.Form formModel ->
            Form.view formModel (FormMsg >> dispatch)

    React.fragment [
        navbar model

        Bulma.container [
            Bulma.columns [
                Bulma.column [
                    column.isOffset3
                    column.is6

                    prop.children [
                        pageContent
                    ]
                ]
            ]
        ]
    ]

open Elmish.React
open Elmish.UrlParser
open Elmish.Navigation
open Elmish.HMR

Program.mkProgram init update view
|> Program.toNavigable (parseHash Router.routeParser) setRoute
|> Viewable.Program.withViewable
|> Program.withTrace ConsoleTracer.withTrace
|> Program.withReactSynchronous "root"
|> Program.run
