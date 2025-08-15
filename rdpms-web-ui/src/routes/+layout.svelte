<script lang="ts">
    import { onMount } from 'svelte';
	import '../app.css';
    import Header from "$lib/layout/Header.svelte";
    import {getOrFetchConfig} from "$lib/util/config-helper";

    onMount(getOrFetchConfig);

	let { children } = $props();
</script>

<Header/>

{@render children()}
