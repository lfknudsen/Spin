module Spinner

open System
open System.Threading
open System.Threading.Channels

let private spinnerCharacters = [|"\r|"; "\r/"; "\r-"; "\r\\"|]

let private delayDurationMs = int (TimeSpan.FromSeconds(0.15).TotalMilliseconds)

// Endlessly loop, updating the current line output each time.
// The current thread sleeps for 0.15 seconds each time.
let private _spin (chars : string array) (wait : ChannelReader<unit>) =
    let mutable idx = 0
    while wait.Count = 0 do
        idx <- idx % chars.Length;
        Console.Write(chars[idx])
        Thread.Sleep(delayDurationMs)
        idx <- idx + 1
    printf "\r"

// Starts a new spinner, which will continue until anything is sent to the
// given channel.
let public spin (wait : ChannelReader<unit>) (_ : obj) : unit =
    _spin spinnerCharacters wait

// Starts a new spinner, which will continue until anything is sent to the
// given channel.
let public spinBackwards (wait : ChannelReader<unit>) (_ : obj) : unit =
    _spin (Array.rev spinnerCharacters) wait

// Creates - but does not start - a background thread which will supply the
// given function with a channel reader.
// Returns the thread and channel writer.
let public createSpinner (f : ChannelReader<unit> -> obj -> unit) =
    let c = Channel.CreateBounded(1)
    let p = ParameterizedThreadStart(f c.Reader)
    let t = Thread p
    (t, c.Writer)

let public startSpinner (f : ChannelReader<unit> -> obj -> unit) : ChannelWriter<unit> =
    let thread, writer = createSpinner f
    thread.Start()
    writer