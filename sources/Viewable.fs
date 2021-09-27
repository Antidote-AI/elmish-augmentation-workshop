module Viewable

open Fable.Core
open Fable.Core.JsInterop
open Elmish
open Browser
open Fable.FontAwesome

[<Literal>]
let private EVENT_IDENTIFIER = "antidote_viewable_program_event"

[<RequireQualifiedAccess>]
module Program =

    open Feliz

    // Viewable msgs
    type Viewable<'msg> =
        // Required to identify the messages coming from the user application
        | UserMsg of 'msg

    type State =
        | Disconnected

    // Viewable program state
    type Model<'model> =
        {
            // Store the User model
            UserModel : 'model
            // Store the Viewable model
            State : State
        }

    let private view (model : Model<'model>) dispatch =
        Html.div [
            prop.className "viewable-container"

            prop.children [
                match model.State with
                | Disconnected ->
                    null
            ]
        ]

    let withViewable (program : Elmish.Program<'arg, 'model, 'msg, 'view>) =
        let mapUpdate update msg model =
            match msg with
            | UserMsg msg ->
                update msg model.UserModel
                |> Tuple.mapFirst (fun userModel -> { model with UserModel = userModel })
                |> Tuple.mapSecond (Cmd.map UserMsg)

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
                    // let action = ev.detail :?> Action

                    // dispatch (ActionReceived action)
                    ()

                window.addEventListener(EVENT_IDENTIFIER, !!window?(EVENT_IDENTIFIER))
            else
            #endif
                window.addEventListener(EVENT_IDENTIFIER, fun ev ->
                    let ev = ev :?> Types.CustomEvent
                    // let action = ev.detail :?> Action

                    // dispatch (ActionReceived action)
                    ()
                )

        let mapSubscribe subscribe model =
            Cmd.batch [
                [ viewableEvent ]
                subscribe model.UserModel |> Cmd.map UserMsg
            ]

        let mapView userView model dispatch =
            React.fragment [
                view model dispatch
                userView model.UserModel (UserMsg >> dispatch)
            ]

        let mapSetState setState model dispatch =
            setState model.UserModel (UserMsg >> dispatch)

        Program.map mapInit mapUpdate mapView mapSetState mapSubscribe program
