import { environment } from "src/environment";

export const URL_LIST={
    //Login
    LOGIN:`${environment.BaseUrl}Login/Login`,
    REGISTER:`${environment.BaseUrl}Login/Register`,

    //Nav
    NAVBAR:`${environment.BaseUrl}Login/GetNavBar`,

    //Stocks
    GET_STOCK_DETAILS:`${environment.BaseUrl}Investment/GetInvestedDetails`,
    ADD_STOCK_DETAILS:`${environment.BaseUrl}Investment/AddInvestmentDetails`,

    //Mf
    GET_MF_DETAILS:`${environment.BaseUrl}Investment/GetInvestedDetails`,
    ADD_MF_DETAILS:`${environment.BaseUrl}Investment/UploadGrowwReport`,
    GET_MF_DAYPERFORMANCE_DETAILS:`${environment.BaseUrl}MutualFund/GetData`,
    GET_SCHEME_NAMES:`${environment.BaseUrl}MutualFund/GetSchemeNames`,

    //Vault
    GET_FILES:`${environment.BaseUrl}Vault/files`,
    VIEW_FILE:`${environment.BaseUrl}files/view`,
    DOWNLOAD_FILE:`${environment.BaseUrl}Vault/download`,
    DELETE_FILE:`${environment.BaseUrl}Vault/files/delete`
}