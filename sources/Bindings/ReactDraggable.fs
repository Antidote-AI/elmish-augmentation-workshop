// ts2fable 0.8.0
module Feliz.ReactDraggable

open Fable.Core
open Fable.Core.JsInterop
open Feliz
open Browser.Types


[<Erase>]
type IDraggableProperty =
    interface end

[<Erase>]
type reactDraggable =

    static member inline draggable (properties: #IDraggableProperty list) =
        Interop.reactApi.createElement(import "default" "react-draggable", createObj !!properties)

    static member inline draggable (_children: #seq<ReactElement>) =
        Interop.reactApi.createElement(import "default" "react-draggable", null)


module Interop =
    let inline mkDraggableAttr (key: string) (value: obj) : IDraggableProperty = unbox (key, value)

type [<StringEnum>] [<RequireQualifiedAccess>] DraggablePropsAxis =
    | Both
    | X
    | Y
    | None

type draggable =
    static member inline axis (value: DraggablePropsAxis) =
        Interop.mkDraggableAttr "axis" value

    static member inline child (child: ReactElement) =
        Interop.mkDraggableAttr "children" child

    // type [<AllowNullLiteral>] DraggableProps =
    //     inherit DraggableCoreProps
    //     abstract axis: DraggablePropsAxis with get, set
    //     abstract bounds: U2<DraggableBounds, string> with get, set
    //     abstract defaultClassName: string with get, set
    //     abstract defaultClassNameDragging: string with get, set
    //     abstract defaultClassNameDragged: string with get, set
    //     abstract defaultPosition: ControlPosition with get, set
    //     abstract positionOffset: PositionOffsetControlPosition with get, set
    //     abstract position: ControlPosition with get, set


    // type [<AllowNullLiteral>] DraggableBounds =
    //     abstract left: float option with get, set
    //     abstract right: float option with get, set
    //     abstract top: float option with get, set
    //     abstract bottom: float option with get, set


    // type DraggableEvent =
    //     // U4<React.MouseEvent<U2<HTMLElement, SVGElement>>, React.TouchEvent<U2<HTMLElement, SVGElement>>, MouseEvent, TouchEvent>
    //     U2<MouseEvent, TouchEvent>

    // type [<AllowNullLiteral>] DraggableEventHandler =
    //     [<Emit "$0($1...)">] abstract Invoke: e: DraggableEvent * data: DraggableData -> unit

    // type [<AllowNullLiteral>] DraggableData =
    //     abstract node: HTMLElement with get, set
    //     abstract x: float with get, set
    //     abstract y: float with get, set
    //     abstract deltaX: float with get, set
    //     abstract deltaY: float with get, set
    //     abstract lastX: float with get, set
    //     abstract lastY: float with get, set

    // type [<AllowNullLiteral>] ControlPosition =
    //     abstract x: float with get, set
    //     abstract y: float with get, set

    // type [<AllowNullLiteral>] PositionOffsetControlPosition =
    //     abstract x: U2<float, string> with get, set
    //     abstract y: U2<float, string> with get, set

    // type [<AllowNullLiteral>] DraggableCoreProps =
    //     abstract allowAnyClick: bool with get, set
    //     abstract cancel: string with get, set
    //     abstract disabled: bool with get, set
    //     abstract enableUserSelectHack: bool with get, set
    //     abstract offsetParent: HTMLElement with get, set
    //     abstract grid: float * float with get, set
    //     abstract handle: string with get, set
    //     abstract nodeRef: IRefValue<HTMLElement> option with get, set
    //     abstract onStart: DraggableEventHandler with get, set
    //     abstract onDrag: DraggableEventHandler with get, set
    //     abstract onStop: DraggableEventHandler with get, set
    //     abstract onMouseDown: (MouseEvent -> unit) with get, set
    //     abstract scale: float with get, set

    // // type [<AllowNullLiteral>] Draggable =
    // //     inherit React.Component<obj, DraggableReactComponent>

    // // type [<AllowNullLiteral>] DraggableStatic =
    // //     [<EmitConstructor>] abstract Create: unit -> Draggable
    // //     abstract defaultProps: DraggableProps with get, set

    // // type [<AllowNullLiteral>] DraggableCore =
    // //     inherit React.Component<obj, DraggableReactComponent>

    // // type [<AllowNullLiteral>] DraggableCoreStatic =
    // //     [<EmitConstructor>] abstract Create: unit -> DraggableCore
    // //     abstract defaultProps: DraggableCoreProps with get, set

    // // type [<AllowNullLiteral>] DraggableReactComponent =
    // //     interface end
