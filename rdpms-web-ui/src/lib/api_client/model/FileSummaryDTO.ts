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

import * as models from './models';

export interface FileSummaryDTO {
    id?: string;

    name?: string;

    contentType?: models.ContentTypeDTO;

    size?: number;

    createdStampUTC?: string;

    deletedStampUTC?: string;

    beginStampUTC?: string;

    endStampUTC?: string;

    isTimeSeries?: boolean;

    isDeleted?: boolean;

}
