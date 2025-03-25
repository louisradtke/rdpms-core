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

import { mapValues } from '../runtime';
/**
 * 
 * @export
 * @interface FileCreateResponseDTO
 */
export interface FileCreateResponseDTO {
    /**
     * 
     * @type {string}
     * @memberof FileCreateResponseDTO
     */
    uploadUri?: string | null;
    /**
     * 
     * @type {string}
     * @memberof FileCreateResponseDTO
     */
    fileId?: string | null;
}

/**
 * Check if a given object implements the FileCreateResponseDTO interface.
 */
export function instanceOfFileCreateResponseDTO(value: object): value is FileCreateResponseDTO {
    return true;
}

export function FileCreateResponseDTOFromJSON(json: any): FileCreateResponseDTO {
    return FileCreateResponseDTOFromJSONTyped(json, false);
}

export function FileCreateResponseDTOFromJSONTyped(json: any, ignoreDiscriminator: boolean): FileCreateResponseDTO {
    if (json == null) {
        return json;
    }
    return {
        
        'uploadUri': json['uploadUri'] == null ? undefined : json['uploadUri'],
        'fileId': json['fileId'] == null ? undefined : json['fileId'],
    };
}

export function FileCreateResponseDTOToJSON(value?: FileCreateResponseDTO | null): any {
    if (value == null) {
        return value;
    }
    return {
        
        'uploadUri': value['uploadUri'],
        'fileId': value['fileId'],
    };
}

