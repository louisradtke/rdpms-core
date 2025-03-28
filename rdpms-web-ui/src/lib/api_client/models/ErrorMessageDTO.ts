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
 * @interface ErrorMessageDTO
 */
export interface ErrorMessageDTO {
    /**
     * The error message. This must be set. If the "user friendly" RDPMS.Core.Server.Model.DTO.V1.ErrorMessageDTO.DisplayMessage is set, this may
     * become technical.
     * @type {string}
     * @memberof ErrorMessageDTO
     */
    message?: string | null;
    /**
     * A message dedicated to the user. If null, RDPMS.Core.Server.Model.DTO.V1.ErrorMessageDTO.Message is the fallback.
     * @type {string}
     * @memberof ErrorMessageDTO
     */
    displayMessage?: string | null;
}

/**
 * Check if a given object implements the ErrorMessageDTO interface.
 */
export function instanceOfErrorMessageDTO(value: object): value is ErrorMessageDTO {
    return true;
}

export function ErrorMessageDTOFromJSON(json: any): ErrorMessageDTO {
    return ErrorMessageDTOFromJSONTyped(json, false);
}

export function ErrorMessageDTOFromJSONTyped(json: any, ignoreDiscriminator: boolean): ErrorMessageDTO {
    if (json == null) {
        return json;
    }
    return {
        
        'message': json['message'] == null ? undefined : json['message'],
        'displayMessage': json['displayMessage'] == null ? undefined : json['displayMessage'],
    };
}

export function ErrorMessageDTOToJSON(value?: ErrorMessageDTO | null): any {
    if (value == null) {
        return value;
    }
    return {
        
        'message': value['message'],
        'displayMessage': value['displayMessage'],
    };
}

