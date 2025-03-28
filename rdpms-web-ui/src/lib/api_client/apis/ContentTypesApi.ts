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
  ContentTypeDTO,
  ProblemDetails,
} from '../models/index';
import {
    ContentTypeDTOFromJSON,
    ContentTypeDTOToJSON,
    ProblemDetailsFromJSON,
    ProblemDetailsToJSON,
} from '../models/index';

export interface ApiV1DataContentTypesBatchPostRequest {
    contentTypeDTO?: Array<ContentTypeDTO>;
}

export interface ApiV1DataContentTypesIdGetRequest {
    id: string;
}

export interface ApiV1DataContentTypesPostRequest {
    contentTypeDTO?: ContentTypeDTO;
}

/**
 * 
 */
export class ContentTypesApi extends runtime.BaseAPI {

    /**
     * Add a batch of content types to the system.
     */
    async apiV1DataContentTypesBatchPostRaw(requestParameters: ApiV1DataContentTypesBatchPostRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<void>> {
        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        const response = await this.request({
            path: `/api/v1/data/content-types/batch`,
            method: 'POST',
            headers: headerParameters,
            query: queryParameters,
            body: requestParameters['contentTypeDTO']!.map(ContentTypeDTOToJSON),
        }, initOverrides);

        return new runtime.VoidApiResponse(response);
    }

    /**
     * Add a batch of content types to the system.
     */
    async apiV1DataContentTypesBatchPost(requestParameters: ApiV1DataContentTypesBatchPostRequest = {}, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<void> {
        await this.apiV1DataContentTypesBatchPostRaw(requestParameters, initOverrides);
    }

    /**
     */
    async apiV1DataContentTypesGetRaw(initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<Array<ContentTypeDTO>>> {
        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        const response = await this.request({
            path: `/api/v1/data/content-types`,
            method: 'GET',
            headers: headerParameters,
            query: queryParameters,
        }, initOverrides);

        return new runtime.JSONApiResponse(response, (jsonValue) => jsonValue.map(ContentTypeDTOFromJSON));
    }

    /**
     */
    async apiV1DataContentTypesGet(initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<Array<ContentTypeDTO>> {
        const response = await this.apiV1DataContentTypesGetRaw(initOverrides);
        return await response.value();
    }

    /**
     */
    async apiV1DataContentTypesIdGetRaw(requestParameters: ApiV1DataContentTypesIdGetRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<ContentTypeDTO>> {
        if (requestParameters['id'] == null) {
            throw new runtime.RequiredError(
                'id',
                'Required parameter "id" was null or undefined when calling apiV1DataContentTypesIdGet().'
            );
        }

        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        const response = await this.request({
            path: `/api/v1/data/content-types/{id}`.replace(`{${"id"}}`, encodeURIComponent(String(requestParameters['id']))),
            method: 'GET',
            headers: headerParameters,
            query: queryParameters,
        }, initOverrides);

        return new runtime.JSONApiResponse(response, (jsonValue) => ContentTypeDTOFromJSON(jsonValue));
    }

    /**
     */
    async apiV1DataContentTypesIdGet(requestParameters: ApiV1DataContentTypesIdGetRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<ContentTypeDTO> {
        const response = await this.apiV1DataContentTypesIdGetRaw(requestParameters, initOverrides);
        return await response.value();
    }

    /**
     * Add a single content type to the system.
     */
    async apiV1DataContentTypesPostRaw(requestParameters: ApiV1DataContentTypesPostRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<void>> {
        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        const response = await this.request({
            path: `/api/v1/data/content-types`,
            method: 'POST',
            headers: headerParameters,
            query: queryParameters,
            body: ContentTypeDTOToJSON(requestParameters['contentTypeDTO']),
        }, initOverrides);

        return new runtime.VoidApiResponse(response);
    }

    /**
     * Add a single content type to the system.
     */
    async apiV1DataContentTypesPost(requestParameters: ApiV1DataContentTypesPostRequest = {}, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<void> {
        await this.apiV1DataContentTypesPostRaw(requestParameters, initOverrides);
    }

}
