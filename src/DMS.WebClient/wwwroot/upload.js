let controller = null;
let paused = false;

export async function uploadStream(file, url, containerId, dotNetObjectRef) {

    const response = await fetch(url, {
        method: "POST",
        body: file, // streams automatically
        headers: {
            "Content-Type": "application/octet-stream",
            "X-File-Name": encodeURIComponent(file.name),
            "X-File-Size": file.size,
            "X-ContainerId": containerId
        },
    });

    if (!response.ok) {
        throw new Error("Upload failed");
    }

    await dotNetObjectRef.invokeMethodAsync('onUploadCompleted');

    //const reader = file.stream().getReader();

    //controller = new AbortController();

    //paused = false;

    //let uploaded = 0;

    //const stream = new ReadableStream({

    //    async pull() {
    //        while (paused) { await new Promise(r => setTimeout(r, 200)); }

    //        const { value, done } = await reader.read();

    //        if (done) {
    //            controllerStream.close();
    //            return;
    //        }

    //        uploaded += value.byteLength;
    //        // onProgress(uploaded, file.size);
    //        onPr
    //        controllerStream.enqueue(value);
    //    }
    //});

    //await fetch(url, {
    //    method: "POST",
    //    headers: {
    //        "Content-Type": "application/octet-stream",
    //        "X-File-Name": encodeURIComponent(file.name),
    //        "X-File-Size": file.size,
    //        "X-ContainerId": containerId
    //    },
    //    body: stream,
    //    signal: controller.signal,
    //    credentials: true
    //});

}

export function pauseUpload() { paused = true; }

export function resumeUpload() { paused = false; }

export function cancelUpload() { controller?.abort(); }
