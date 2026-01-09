const controller = new AbortController();
const signal = controller.signal;

let inputElement;
let _ref;
let _containerId;
let _url;

export function initializeUploader(elementId, dotNetRef) {
    _ref = dotNetRef;
    inputElement = document.getElementById(elementId);
    inputElement.addEventListener('change', async (event) => {
        console.log("File input changed");
        try {
            var files = event.target.files;

            if (files.length > 0) {
                for (const file of files) {

                    if (_url) {
                        let response = await fetch(_url, {
                            method: 'POST',
                            body: file,
                            headers: {
                                "Content-Type": "application/octet-stream",
                                "X-File-Name": encodeURIComponent(file.name),
                                "X-File-Size": file.size,
                                "X-ContainerId": _containerId,
                                "X-File-Type": file.type,
                                "Content-Length": file.size,
                                "X-Last-Modified": file.lastModified ? new Date(file.lastModified).toUTCString() : new Date().toUTCString()
                            }
                        });

                        if (response.ok) {
                            await _ref.invokeMethodAsync('onFileUploadCompleted', true);
                        } else {
                            await _ref.invokeMethodAsync('onFileUploadCompleted', false);
                        }
                    }
                }
            }
        } finally {
            controller.abort();
        }
    }, { signal });
}
export function uploadFile(url, containerId) {

    _containerId = containerId;
    _url = url;
    if (inputElement) {
        inputElement.value = null; // Reset the input value to allow re-uploading the same file

        inputElement.click();
    }
}   