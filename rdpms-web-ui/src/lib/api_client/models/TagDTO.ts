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
 * @interface TagDTO
 */
export interface TagDTO {
    /**
     * 
     * @type {string}
     * @memberof TagDTO
     */
    id?: string | null;
    /**
     * 
     * @type {string}
     * @memberof TagDTO
     */
    name?: string | null;
}

/**
 * Check if a given object implements the TagDTO interface.
 */
export function instanceOfTagDTO(value: object): value is TagDTO {
    return true;
}

export function TagDTOFromJSON(json: any): TagDTO {
    return TagDTOFromJSONTyped(json, false);
}

export function TagDTOFromJSONTyped(json: any, ignoreDiscriminator: boolean): TagDTO {
    if (json == null) {
        return json;
    }
    return {
        
        'id': json['id'] == null ? undefined : json['id'],
        'name': json['name'] == null ? undefined : json['name'],
    };
}

export function TagDTOToJSON(value?: TagDTO | null): any {
    if (value == null) {
        return value;
    }
    return {
        
        'id': value['id'],
        'name': value['name'],
    };
}

