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
    | SendDisconnect

// Standard Model for an Elmish application
type Model =
    {
        // Store the current route
        CurrentRoute : Router.Route option
        // Store the current Page this is where most of the application state is
        ActivePage : Page
    }

// Given a route, it will return the appropriate Page
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

    | SendDisconnect ->
        model
        , Viewable.disconnect

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

let private showConnectButtons dispatch =
    Bulma.columns [
        Bulma.column [
            Bulma.button.button [
                color.isPrimary
                button.isMedium

                prop.onClick (fun _ ->
                    dispatch StartCall
                )

                prop.text "Start call (fullscreen)"
            ]
        ]

        Bulma.column [
            Bulma.button.button [
                color.isPrimary
                button.isMedium

                prop.onClick (fun _ ->
                    dispatch SendDisconnect
                )

                prop.text "Disconnect"
            ]
        ]
    ]

[<ReactComponent>]
let Home dispatch =
    let viewableState = React.useContext Viewable.Program.viewableContext

    Bulma.hero [
        hero.isFullHeightWithNavbar

        prop.children [
            Bulma.heroBody [
                helpers.isJustifyContentCenter

                prop.children [
                    match viewableState with
                    | Viewable.Program.Connected _ ->
                        Html.text "Connected"

                    | Viewable.Program.Disconnected ->
                        showConnectButtons dispatch
                ]
            ]
        ]
    ]

let private view (model : Model) (dispatch : Dispatch<Msg>) =
    let pageContent =
        match model.ActivePage with
        | Page.Home ->
            Home dispatch

        | Page.NotFound ->
            Html.text "Not found"

        | Page.Form formModel ->
            Form.view formModel (FormMsg >> dispatch)

    React.fragment [
        navbar model

        pageContent

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
