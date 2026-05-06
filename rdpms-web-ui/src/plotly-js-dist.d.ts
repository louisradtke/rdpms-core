declare module "plotly.js-dist-min" {
    import type { Config, Data, Layout, PlotlyHTMLElement } from "plotly.js";

    const Plotly: {
        newPlot: (
            root: HTMLElement,
            data: Partial<Data>[],
            layout?: Partial<Layout>,
            config?: Partial<Config>
        ) => Promise<PlotlyHTMLElement>;
        purge: (root: HTMLElement) => void;
    };

    export default Plotly;
}
