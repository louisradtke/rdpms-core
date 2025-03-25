/* tslint:disable */
/* eslint-disable */
/**
 * My API v1
 * API Version v1
 *
 * The version of the OpenAPI document: v1
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */


import * as runtime from '../runtime';
import type {
  DataStoreSummaryDTO,
  ProblemDetails,
} from '../models/index';
import {
    DataStoreSummaryDTOFromJSON,
    DataStoreSummaryDTOToJSON,
    ProblemDetailsFromJSON,
    ProblemDetailsToJSON,
} from '../models/index';

export interface ApiV1DataStoresPostRequest {
    dataStoreSummaryDTO?: DataStoreSummaryDTO;
}

/**
 * 
 */
export class StoresApi extends runtime.BaseAPI {

    /**
     */
    async apiV1DataStoresGetRaw(initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<Array<DataStoreSummaryDTO>>> {
        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        const response = await this.request({
            path: `/api/v1/data/stores`,
            method: 'GET',
            headers: headerParameters,
            query: queryParameters,
        }, initOverrides);

        return new runtime.JSONApiResponse(response, (jsonValue) => jsonValue.map(DataStoreSummaryDTOFromJSON));
    }

    /**
     */
    async apiV1DataStoresGet(initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<Array<DataStoreSummaryDTO>> {
        const response = await this.apiV1DataStoresGetRaw(initOverrides);
        return await response.value();
    }

    /**
     */
    async apiV1DataStoresPostRaw(requestParameters: ApiV1DataStoresPostRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<void>> {
        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        const response = await this.request({
            path: `/api/v1/data/stores`,
            method: 'POST',
            headers: headerParameters,
            query: queryParameters,
            body: DataStoreSummaryDTOToJSON(requestParameters['dataStoreSummaryDTO']),
        }, initOverrides);

        return new runtime.VoidApiResponse(response);
    }

    /**
     */
    async apiV1DataStoresPost(requestParameters: ApiV1DataStoresPostRequest = {}, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<void> {
        await this.apiV1DataStoresPostRaw(requestParameters, initOverrides);
    }

}
