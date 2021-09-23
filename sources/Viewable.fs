module Viewable

open Elmish

let cmd = ""

[<RequireQualifiedAccess>]
module Program =

    open Feliz

    type Viewable<'msg> =
        | UserMsg of 'msg

    type DisplayMode =
        | FullScreen
        | Floating

    type Model<'model> =
        {
            UserModel : 'model
            DisplayMode : DisplayMode
        }

    let private view (model : Model<'model>) dispatch =
        Html.div [
            prop.className "viewable-container"
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
                DisplayMode = FullScreen
            }
            , cmd

        let mapInit init =
            init
            >> (fun (model, cmd) ->
                model
                , cmd |> Cmd.map UserMsg
            )
            >> createModel

        let mapSubscribe subscribe model =
            subscribe model.UserModel |> Cmd.map UserMsg

        let mapView userView model dispatch =
            React.fragment [
                view model dispatch
                userView model.UserModel (UserMsg >> dispatch)
            ]

        let mapSetState setState model dispatch =
            setState model.UserModel (UserMsg >> dispatch)

        Program.map mapInit mapUpdate mapView mapSetState mapSubscribe program
