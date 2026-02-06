<script lang="ts">
    import { onMount } from 'svelte';
	import '../app.css';
    import Header from "$lib/layout/Header.svelte";
    import {getOrFetchConfig} from "$lib/util/config-helper";

    onMount(getOrFetchConfig);

	let { children } = $props();
</script>

<!-- body with class="h-screen overflow-hidden" is wrapped in app.html -->

<Header/>

<!-- Body below header: sidebar + main -->
<div class="flex h-[calc(100vh-4rem)] overflow-y-auto">

    {@render children()}

</div>
