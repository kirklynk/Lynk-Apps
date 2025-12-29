const controller = new AbortController();
export function uploadFile(elementId, url, containerId, dotNetRef) {

    const signal = controller.signal;

    let inputElement = document.getElementById(elementId);
    if (inputElement) {
        inputElement.value = null; // Reset the input value to allow re-uploading the same file
        inputElement.addEventListener('change', async (event) => {
            try {
                var files = event.target.files;

                if (files.length > 0) {
                    for (const file of files) {

                        if (url) {
                            let response = await fetch(url, {
                                method: 'POST',
                                body: file,
                                headers: {
                                    "Content-Type": "application/octet-stream",
                                    "X-File-Name": encodeURIComponent(file.name),
                                    "X-File-Size": file.size,
                                    "X-ContainerId": containerId,
                                    "X-File-Type": file.type,
                                    "Content-Length": file.size,
                                    "X-Last-Modified": file.lastModified ? new Date(file.lastModified).toUTCString() : new Date().toUTCString()
                                }
                            });
                            
                            if (response.ok) {
                                await dotNetRef.invokeMethodAsync('onFileUploadCompleted', true);
                            } else {
                                await dotNetRef.invokeMethodAsync('onFileUploadCompleted', false);
                            }
                        }
                    }
                }
            } finally {
                controller.abort();
            }

        }, { signal });
        inputElement.click();
    }
}   