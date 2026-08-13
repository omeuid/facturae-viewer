// Copyright (c) 2026 Facturae Viewer contributors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0

using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace Facturae.App.Services;

/// <summary>
/// Garantiza una única instancia de la aplicación. Si ya hay una instancia en
/// ejecución, la segunda le envía la ruta recibida por línea de comandos y se
/// cierra, permitiendo abrir el fichero en la ventana existente.
/// </summary>
public static class SingleInstance
{
    private const string MutexName = @"Local\FacturaeViewer.SingleInstance";
    private const string PipeName = "FacturaeViewer.FileOpen";
    private static CancellationTokenSource? _cancellation;

    /// <summary>
    /// Intenta adquirir la instancia principal. Devuelve true si esta es la
    /// primera instancia (continuar el arranque) o false si otra ya está
    /// activa (enviar la ruta y salir).
    /// </summary>
    public static bool TryAcquire(string? fileArg, Action<string> onFileReceived)
    {
        var mutex = new Mutex(initiallyOwned: true, MutexName, out bool createdNew);
        if (!createdNew)
        {
            NotifyExistingInstance(fileArg);
            mutex.Dispose();
            return false;
        }

        GC.KeepAlive(mutex);
        StartListener(onFileReceived);
        return true;
    }

    private static void NotifyExistingInstance(string? fileArg)
    {
        if (string.IsNullOrEmpty(fileArg))
            return;

        try
        {
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            client.Connect(TimeSpan.FromSeconds(2));
            using var writer = new StreamWriter(client, new UTF8Encoding(false));
            writer.WriteLine(fileArg);
            writer.Flush();
        }
        catch (IOException)
        {
            // La instancia existente no escucha; se ignora.
        }
    }

    private static void StartListener(Action<string> onFileReceived)
    {
        _cancellation = new CancellationTokenSource();
        _ = Task.Run(() => ListenerLoop(onFileReceived, _cancellation.Token));
    }

    /// <summary>Detiene la escucha de instancias secundarias (al salir).</summary>
    public static void Stop()
    {
        _cancellation?.Cancel();
    }

    private static async Task ListenerLoop(Action<string> onFileReceived, CancellationToken token)
    {        while (!token.IsCancellationRequested)
        {
            try
            {
                using var server = new NamedPipeServerStream(
                    PipeName,
                    PipeDirection.In,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                await server.WaitForConnectionAsync(token);
                using var reader = new StreamReader(server, new UTF8Encoding(false));
                var path = await reader.ReadLineAsync(token);
                if (!string.IsNullOrEmpty(path))
                    System.Windows.Application.Current.Dispatcher.Invoke(() => onFileReceived(path));
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
                // Reintenta en la siguiente iteración del bucle.
            }
        }
    }
}