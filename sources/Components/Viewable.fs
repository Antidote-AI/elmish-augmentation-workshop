module Components.Viewable

open Feliz
open Feliz.Bulma
open Feliz.UseElmish
open Browser
open System
open Elmish

let usePortal (query : string) =
    let rootElmentRef : IRefValue<Types.HTMLElement> = React.useRef(null)

    let getRootElement () =
        if rootElmentRef.current = null then
            rootElmentRef.current <- document.createElement("div")

        rootElmentRef.current

    React.useEffect(fun () ->
        let parentElem = document.querySelector(query)

        parentElem.appendChild(rootElmentRef.current)
        |> ignore

        { new IDisposable
            with member __.Dispose() = rootElmentRef.current.remove()
        }

    , [| box query |] )

    getRootElement ()

type Msg =
    | Connect
    | Disconnect

type ConnectedState =
    | FullScreen
    | Floating

type Model =
    | Disconnected
    | Connected of ConnectedState

let init () =
    Disconnected
    , Cmd.none

let update (msg : Msg) (model : Model) =
    match msg with
    | Connect ->
        match model with
        | Disconnected ->
            Connected FullScreen
            , Cmd.none

        | Connected _ ->
            model
            , Cmd.none

    | Disconnect ->
        Disconnected
        , Cmd.none

let content (state : Model) dispatch =
    Html.button [
        match state with
        | Connected _ ->
            prop.text "Disconnect"
            prop.onClick (fun _ ->
                dispatch Disconnect
            )

        | Disconnected ->
            prop.text "Connect"
            prop.onClick (fun _ ->
                dispatch Connect
            )
    ]

[<ReactComponent>]
let Viewable () =
    let state, dispatch = React.useElmish(init, update, [||])

    let target = usePortal("#antidote-video-container")

    ReactDOM.createPortal(
        content state dispatch,
        target
    )
