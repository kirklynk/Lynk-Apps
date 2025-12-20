export function triggerFileInput(elementId, url, containerId, dotNetRef) {

    const controller = new AbortController();
    const signal = controller.signal;

    let inputElement = document.getElementById(elementId);
    if (inputElement) {
        inputElement.value = null; // Reset the input value to allow re-uploading the same file
        inputElement.addEventListener('change', async (event) => {
            try {
                var files = event.target.files;
                console.log(files);
                if (files.length > 0) {
                    var file = files[0];
                    if (url) {
                        let response = await fetch(url, {
                            method: 'POST',
                            body: file.stream ? file.stream : file,
                            headers: {
                                "Content-Type": "application/octet-stream",
                                "X-File-Name": encodeURIComponent(file.name),
                                "X-File-Size": file.size,
                                "X-ContainerId": containerId,
                                "X-File-Type": file.type
                            }
                        });
                        console.log(response);
                        if (response.ok) {
                            await dotNetRef.invokeMethodAsync('onFileUploadCompleted', true);
                        } else {
                            await dotNetRef.invokeMethodAsync('onFileUploadCompleted', false);
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