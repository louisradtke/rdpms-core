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
  ContainerSummaryDTO,
  ProblemDetails,
} from '../models/index';
import {
    ContainerSummaryDTOFromJSON,
    ContainerSummaryDTOToJSON,
    ProblemDetailsFromJSON,
    ProblemDetailsToJSON,
} from '../models/index';

export interface ApiV1DataContainersPostRequest {
    containerSummaryDTO?: ContainerSummaryDTO;
}

/**
 * 
 */
export class ContainersApi extends runtime.BaseAPI {

    /**
     */
    async apiV1DataContainersGetRaw(initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<Array<ContainerSummaryDTO>>> {
        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        const response = await this.request({
            path: `/api/v1/data/containers`,
            method: 'GET',
            headers: headerParameters,
            query: queryParameters,
        }, initOverrides);

        return new runtime.JSONApiResponse(response, (jsonValue) => jsonValue.map(ContainerSummaryDTOFromJSON));
    }

    /**
     */
    async apiV1DataContainersGet(initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<Array<ContainerSummaryDTO>> {
        const response = await this.apiV1DataContainersGetRaw(initOverrides);
        return await response.value();
    }

    /**
     * Add a single item to the system.
     */
    async apiV1DataContainersPostRaw(requestParameters: ApiV1DataContainersPostRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<void>> {
        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        const response = await this.request({
            path: `/api/v1/data/containers`,
            method: 'POST',
            headers: headerParameters,
            query: queryParameters,
            body: ContainerSummaryDTOToJSON(requestParameters['containerSummaryDTO']),
        }, initOverrides);

        return new runtime.VoidApiResponse(response);
    }

    /**
     * Add a single item to the system.
     */
    async apiV1DataContainersPost(requestParameters: ApiV1DataContainersPostRequest = {}, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<void> {
        await this.apiV1DataContainersPostRaw(requestParameters, initOverrides);
    }

}
