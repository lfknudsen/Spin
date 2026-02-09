module Spinner

open System.Threading
open System.Threading.Channels

let private spinnerCharacters = [|'|'; '/'; '-'; '\\'|]

// Endlessly loop, updating the current line output each time.
// The current thread sleeps for 0.15 seconds each time.
let rec private _spin (chars : char array) (i : int) (wait : ChannelReader<unit>) =
    if wait.Count = 0 then
        let idx = i % chars.Length;
        printf $"\r%c{chars[idx]}"
        Thread.Sleep(System.TimeSpan.FromSeconds(0.15))
        _spin chars (idx + 1) wait
    else
        printf "\r"

// Starts a new spinner, which will continue until anything is sent to the
// given channel.
let public spin (wait : ChannelReader<unit>) (_ : obj) : unit =
    _spin spinnerCharacters 0 wait

// Starts a new spinner, which will continue until anything is sent to the
// given channel.
let public spinBackwards (wait : ChannelReader<unit>) (_ : obj) : unit =
    _spin (Array.rev spinnerCharacters) 0 wait

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