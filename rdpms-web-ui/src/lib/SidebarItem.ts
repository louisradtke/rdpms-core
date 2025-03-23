/**
 * Interface representing an item in a sidebar navigation structure.
 */
export interface SidebarItem {
    /**
     * Represents a text label that can be used for identification, naming, or description purposes.
     */
    label: string;
    /**
     * Represents the value to be assigned to the `href` attribute of an HTML anchor element.
     *
     * This variable typically defines a fragment identifier that specifies
     * the destination of the actual link.
     */
    hrefValue: string;
    /**
     * A text string that provides additional information about a user interface element.
     * This text is typically displayed when the user hovers over or interacts with the related element.
     * It is optional and, if not provided, no tooltip will be displayed.
     */
    tooltip?: string;
}