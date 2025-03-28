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


/**
 * Enum indicating whether a dataset was just created or all associated files
 * were linked. Remember design decision: datasets shall be immutable.
 * @export
 */
export const DataSetStateDTO = {
    NUMBER_0: 0,
    NUMBER_1: 1
} as const;
export type DataSetStateDTO = typeof DataSetStateDTO[keyof typeof DataSetStateDTO];


export function instanceOfDataSetStateDTO(value: any): boolean {
    for (const key in DataSetStateDTO) {
        if (Object.prototype.hasOwnProperty.call(DataSetStateDTO, key)) {
            if ((DataSetStateDTO as Record<string, DataSetStateDTO>)[key] === value) {
                return true;
            }
        }
    }
    return false;
}

export function DataSetStateDTOFromJSON(json: any): DataSetStateDTO {
    return DataSetStateDTOFromJSONTyped(json, false);
}

export function DataSetStateDTOFromJSONTyped(json: any, ignoreDiscriminator: boolean): DataSetStateDTO {
    return json as DataSetStateDTO;
}

export function DataSetStateDTOToJSON(value?: DataSetStateDTO | null): any {
    return value as any;
}

