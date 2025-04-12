export const serverAddress = "https://localhost:7210/api";
export const urls = {
    auth: {
        login: serverAddress + "/auth/login",
        register: serverAddress + "/auth/register",
        verifyAdmin: serverAddress + "/auth/verify-admin"
    },
    users: serverAddress + "/users",
    userVersions: serverAddress + "/userversions",
    roles: serverAddress + "/roles"
}