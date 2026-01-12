let _containerId;

export function openFileInput(inputElement, containerId) {
    inputElement.click();
    _containerId = containerId;
}

export function uploadAsync(inputElement, uploadUrl, dotNetHelper) {
        return new Promise((resolve) => {
            const file = inputElement.files[0];
            if (!file) return;

            const xhr = new XMLHttpRequest();
            const formData = new FormData();
            formData.append("file", file);
            formData.append("container", _containerId);
            console.log("Uploading file:", file.name, "to", uploadUrl);
            
            // Progress tracking
            xhr.upload.onprogress = (e) => {
                if (e.lengthComputable) {
                    const percent = Math.round((e.loaded / e.total) * 100);
                    dotNetHelper.invokeMethodAsync('UpdateProgress', percent, file.name);
                }
            };

            // Completion handling
            xhr.onload = () => {
                if (xhr.status >= 200 && xhr.status < 300) {
                    dotNetHelper.invokeMethodAsync('OnUploadComplete', true, "Upload Finished!");
                } else {
                    dotNetHelper.invokeMethodAsync('OnUploadComplete', false, "Server Error: " + xhr.status);
                }
            };

            xhr.open("POST", uploadUrl);
            xhr.send(formData);
        });
}

