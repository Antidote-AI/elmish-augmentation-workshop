module Viewable

open Fable.Core
open Fable.Core.JsInterop
open Elmish
open Browser
open Fable.FontAwesome

[<Literal>]
let private EVENT_IDENTIFIER = "antidote_viewable_program_event"

type Action =
    | Connect
    | Disconnect
    | ForceFloating

let connect =
    [
        fun _ ->
            let detail =
                jsOptions<Types.CustomEventInit>(fun o ->
                    o.detail <- Action.Connect
                )

            let event = CustomEvent.Create(EVENT_IDENTIFIER, detail)

            window.dispatchEvent(event)
            |> ignore
    ]

let disconnect =
    [
        fun _ ->
            let detail =
                jsOptions<Types.CustomEventInit>(fun o ->
                    o.detail <- Action.Disconnect
                )

            let event = CustomEvent.Create(EVENT_IDENTIFIER, detail)

            window.dispatchEvent(event)
            |> ignore
    ]

let forceFloating =
    [
        fun _ ->
            let detail =
                jsOptions<Types.CustomEventInit>(fun o ->
                    o.detail <- Action.ForceFloating
                )

            let event = CustomEvent.Create(EVENT_IDENTIFIER, detail)

            window.dispatchEvent(event)
            |> ignore
    ]

[<RequireQualifiedAccess>]
module Program =

    open Feliz
    open Feliz.Bulma
    open Feliz.ReactDraggable

    // Viewable msgs
    type Viewable<'msg> =
        // Required to identify the messages coming from the user application
        | UserMsg of 'msg
        | ActionReceived of Action
        | EndCall
        | SwitchToFloating
        | SwitchToFullScreen

    type ConnectedState =
        | FullScreen
        | Floating

    type State =
        | Connected of ConnectedState
        | Disconnected

    // Viewable program state
    type Model<'model> =
        {
            // Store the User model
            UserModel : 'model
            // Store the Viewable model
            State : State
        }

    let viewableContext : Fable.React.IContext<State> = React.createContext()

    [<ReactComponent>]
    let ViewableProvider (state : State) (children : seq<ReactElement>) =
        React.contextProvider(viewableContext, state, children)

    module private Control =

        let endCall dispatch =
            Bulma.button.span [
                color.isDanger
                prop.onClick (fun _ ->
                    dispatch EndCall
                )

                prop.children [
                    Bulma.icon [
                        color.isWhite

                        prop.children [
                            Fa.i
                                [
                                    Fa.Solid.PhoneSlash
                                ]
                                [ ]
                        ]
                    ]
                ]
            ]

        let maximize dispatch =
            Bulma.button.span [
                prop.onClick (fun _ ->
                    dispatch SwitchToFullScreen
                )

                prop.children [
                    Bulma.icon [
                        Fa.i
                            [
                                Fa.Solid.Expand
                            ]
                            [ ]
                    ]
                ]
            ]

        let minimize dispatch =
            Bulma.button.span [
                prop.onClick (fun _ ->
                    dispatch SwitchToFloating
                )

                prop.children [
                    Bulma.icon [
                        Fa.i
                            [
                                Fa.Solid.Compress
                            ]
                            [ ]
                    ]
                ]
            ]

    let renderFullScreen dispatch =
        Html.div [
            prop.className "antidote-video is-fullscreen"

            prop.children [
                Html.div [
                    prop.className "antidote-video-controls"

                    prop.children [

                        Bulma.buttons [
                            Control.minimize dispatch

                            Control.endCall dispatch
                        ]
                    ]
                ]
            ]
        ]

    let renderFloating dispatch =
        let content =
            Html.div [
                prop.classes [
                    "antidote-video is-floating"
                ]

                prop.children [
                    Html.div [
                        prop.className "antidote-video-controls"

                        prop.children [

                            Bulma.buttons [
                                Control.maximize dispatch

                                Control.endCall dispatch
                            ]
                        ]
                    ]
                ]
            ]

        // Make the content draggable
        reactDraggable.draggable [

            draggable.child content
        ]

    let private view (model : Model<'model>) dispatch =
        Html.div [
            prop.className "viewable-container"

            prop.children [
                match model.State with
                | Disconnected ->
                    null

                | Connected connectedState ->
                    match connectedState with
                    | FullScreen ->
                        renderFullScreen dispatch

                    | Floating ->
                        renderFloating dispatch
            ]
        ]

    let private applyIfConnected (model : Model<_>) (func : ConnectedState -> ConnectedState * Cmd<Viewable<_>>) =
        match model.State with
        | Connected connectedState ->
            func connectedState
            |> Tuple.mapFirst Connected
            |> Tuple.mapFirst (fun connectedState -> { model with State = connectedState })

        | Disconnected ->
            model
            , Cmd.none

    let withViewable (program : Elmish.Program<'arg, 'model, 'msg, 'view>) =
        let mapUpdate update msg model =
            match msg with
            | UserMsg msg ->
                update msg model.UserModel
                |> Tuple.mapFirst (fun userModel -> { model with UserModel = userModel })
                |> Tuple.mapSecond (Cmd.map UserMsg)

            | ActionReceived action ->
                match action with
                | Connect ->
                    { model with
                        State = Connected FullScreen
                    }
                    , Cmd.none

                | Disconnect ->
                    { model with
                        State = Disconnected
                    }
                    , Cmd.none

                | ForceFloating ->
                    applyIfConnected
                        model
                        (fun connectedState ->
                            match connectedState with
                            | FullScreen ->
                                Floating
                                , Cmd.none

                            // Already floating do nothing
                            | Floating _ ->
                                connectedState
                                , Cmd.none
                        )

            | EndCall ->
                { model with
                    State = Disconnected
                }
                , Cmd.none

            | SwitchToFloating ->
                applyIfConnected
                    model
                    (fun connectedState ->
                        match connectedState with
                        | FullScreen ->
                            Floating
                            , Cmd.none

                        // Already floating do nothing
                        | Floating _ ->
                            connectedState
                            , Cmd.none
                    )

            | SwitchToFullScreen ->
                { model with
                    State = Connected FullScreen
                }
                , Cmd.none

        let createModel (model, cmd) =
            {
                UserModel = model
                State = Disconnected
            }
            , cmd

        let mapInit init =
            init
            >> (fun (model, cmd) ->
                model
                , cmd |> Cmd.map UserMsg
            )
            >> createModel

        let viewableEvent (dispatch : Dispatch<Viewable<_>>) =
            // If HMR support is active, then we provide have a custom implementation.
            // This is needed to avoid:
            // - flickering (trigger several react renderer process)
            // - attaching several event listener to the same event
            #if DEBUG
            let hot = HMR.``module``.hot

            if not (isNull hot) then
                if hot.status() <> HMR.Status.Idle then
                    window.removeEventListener(EVENT_IDENTIFIER, !!window?(EVENT_IDENTIFIER))

                window?(EVENT_IDENTIFIER) <- fun (ev : Types.Event) ->
                    let ev = ev :?> Types.CustomEvent
                    let action = ev.detail :?> Action

                    dispatch (ActionReceived action)

                window.addEventListener(EVENT_IDENTIFIER, !!window?(EVENT_IDENTIFIER))
            else
            #endif
                window.addEventListener(EVENT_IDENTIFIER, fun ev ->
                    let ev = ev :?> Types.CustomEvent
                    let action = ev.detail :?> Action

                    dispatch (ActionReceived action)
                )

        let mapSubscribe subscribe model =
            Cmd.batch [
                [ viewableEvent ]
                subscribe model.UserModel |> Cmd.map UserMsg
            ]

        let mapView userView model dispatch =
            ViewableProvider model.State [
                view model dispatch
                userView model.UserModel (UserMsg >> dispatch)
            ]

        let mapSetState setState model dispatch =
            setState model.UserModel (UserMsg >> dispatch)

        Program.map mapInit mapUpdate mapView mapSetState mapSubscribe program
