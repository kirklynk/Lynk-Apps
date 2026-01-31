let _containerId;

const sleep = (ms) => new Promise((resolve) => setTimeout(resolve, ms));
export function openFileInput(inputElement, containerId) {
    inputElement.value = null; // Reset the input
    inputElement.click();
    _containerId = containerId;
}

export function uploadAsync(inputElement, uploadUrl, dotNetHelper) {
    return new Promise(async (resolve) => {
        await dotNetHelper.invokeMethodAsync('resetProgress');
        const files = Array.from(inputElement.files);
        if (files.length === 0) {
            return;
        }

        var uploadedFiles = [];
        files.forEach(file => {
            uploadedFiles.push({
                name: file.name,
                status: "Waiting",
                progress: 0
            });
        });

        await dotNetHelper.invokeMethodAsync('onUploadStarted', uploadedFiles);

        const xhr = new XMLHttpRequest();
        let index = 0;
        for (let index = 0; index < files.length; index++) {

            const selectedFile = files[index];

            dotNetHelper.invokeMethodAsync('updateProgress', selectedFile.name, 'Uploading', 0);

            const formData = new FormData();

            formData.append("file", selectedFile);
            formData.append("container", _containerId);

            xhr.upload.onprogress = (e) => {
                if (e.lengthComputable) {
                    const percent = Math.round((e.loaded / e.total) * 100);
                    dotNetHelper.invokeMethodAsync('updateProgress', selectedFile.name, 'Uploading', percent);
                }
            };

            xhr.onload = () => {
                uploadedFiles.push({ file: selectedFile.name });
                if (xhr.status >= 200 && xhr.status < 300) {
                    dotNetHelper.invokeMethodAsync('onFileUploaded', selectedFile.name, 'Completed');
                } else {
                    dotNetHelper.invokeMethodAsync('onFileUploaded', selectedFile.name, 'Failed');
                }
            };

            xhr.open("POST", uploadUrl);
            xhr.send(formData);

            while (xhr.readyState !== XMLHttpRequest.DONE) {
                await sleep(50);
            }
        }

        resolve();
        await dotNetHelper.invokeMethodAsync('onFileUploadCompleted');
    });
}

