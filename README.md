# Spin

A small library - as well as an application which uses it - 
to draw a "spinner" in the console while something else is
executing.

## Spinner

This is the central library. Use this in your own programmes to draw a spinner.
Tell it to stop by writing to the channel writer returned on creation.

### Usage

Start a background thread, with the spinner continuously rendering to the
standard output. When the execution is finished, stop and clean up by
calling `ChannelWriter.WriteAsync`:
```fsharp
let thread, writer = Spinner.createSpinner Spinner.spin
// ...
thread.Start()
// ...
writer.WriteAsync(()) |> ignore
```

Alternatively, start it directly:
```fsharp
let writer = Spinner.startSpinner Spinner.spin
// ...
writer.WriteAsync(()) |> ignore
```

## Spin

This is the executable. Call it from the command-line before the rest of your
command, and it will start a spinner, execute the command, and then stop the
spinner afterward.

### Usage

```
spin [options] <programme to execute> [<programme arguments>...]

    Options:
        -r
        --reverse           Reverse the direction of the spinner graphic.
        -h
        --help              Print this message and exit.
```

For example, if Spin has been built to an executable on the PATH:

```
Spin sleep 10
```