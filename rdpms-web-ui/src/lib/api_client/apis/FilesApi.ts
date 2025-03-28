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
  ErrorMessageDTO,
  FileCreateRequestDTO,
  FileCreateResponseDTO,
  FileSummaryDTO,
} from '../models/index';
import {
    ErrorMessageDTOFromJSON,
    ErrorMessageDTOToJSON,
    FileCreateRequestDTOFromJSON,
    FileCreateRequestDTOToJSON,
    FileCreateResponseDTOFromJSON,
    FileCreateResponseDTOToJSON,
    FileSummaryDTOFromJSON,
    FileSummaryDTOToJSON,
} from '../models/index';

export interface ApiV1DataFilesPostRequest {
    fileCreateRequestDTO?: FileCreateRequestDTO;
}

/**
 * 
 */
export class FilesApi extends runtime.BaseAPI {

    /**
     */
    async apiV1DataFilesGetRaw(initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<Array<FileSummaryDTO>>> {
        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        const response = await this.request({
            path: `/api/v1/data/files`,
            method: 'GET',
            headers: headerParameters,
            query: queryParameters,
        }, initOverrides);

        return new runtime.JSONApiResponse(response, (jsonValue) => jsonValue.map(FileSummaryDTOFromJSON));
    }

    /**
     */
    async apiV1DataFilesGet(initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<Array<FileSummaryDTO>> {
        const response = await this.apiV1DataFilesGetRaw(initOverrides);
        return await response.value();
    }

    /**
     */
    async apiV1DataFilesPostRaw(requestParameters: ApiV1DataFilesPostRequest, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<runtime.ApiResponse<FileCreateResponseDTO>> {
        const queryParameters: any = {};

        const headerParameters: runtime.HTTPHeaders = {};

        headerParameters['Content-Type'] = 'application/json';

        const response = await this.request({
            path: `/api/v1/data/files`,
            method: 'POST',
            headers: headerParameters,
            query: queryParameters,
            body: FileCreateRequestDTOToJSON(requestParameters['fileCreateRequestDTO']),
        }, initOverrides);

        return new runtime.JSONApiResponse(response, (jsonValue) => FileCreateResponseDTOFromJSON(jsonValue));
    }

    /**
     */
    async apiV1DataFilesPost(requestParameters: ApiV1DataFilesPostRequest = {}, initOverrides?: RequestInit | runtime.InitOverrideFunction): Promise<FileCreateResponseDTO> {
        const response = await this.apiV1DataFilesPostRaw(requestParameters, initOverrides);
        return await response.value();
    }

}
