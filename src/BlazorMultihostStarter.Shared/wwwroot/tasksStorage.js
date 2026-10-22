const databaseName = "BlazorMultihostStarter";
const objectStoreName = "taskModes";

function openDatabase() {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open(databaseName, 1);
        request.onupgradeneeded = () => request.result.createObjectStore(objectStoreName);
        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error ?? new Error("Could not open local task storage."));
    });
}

export async function read(key) {
    const database = await openDatabase();
    return new Promise((resolve, reject) => {
        const request = database.transaction(objectStoreName, "readonly")
            .objectStore(objectStoreName)
            .get(key);
        request.onsuccess = () => resolve(request.result ?? null);
        request.onerror = () => reject(request.error ?? new Error("Could not read local task storage."));
    });
}

export async function write(key, value) {
    const database = await openDatabase();
    return new Promise((resolve, reject) => {
        const transaction = database.transaction(objectStoreName, "readwrite");
        transaction.objectStore(objectStoreName).put(value, key);
        transaction.oncomplete = () => resolve();
        transaction.onerror = () => reject(transaction.error ?? new Error("Could not save local tasks."));
        transaction.onabort = () => reject(transaction.error ?? new Error("Local task storage transaction was aborted."));
    });
}
