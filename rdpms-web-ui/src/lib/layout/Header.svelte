<script lang="ts">
    import {onMount} from 'svelte';

    // Manage the state for the burger menu
    let showMenu = false;

    // Example links
    const links = [
        {name: 'Projects', href: '/projects'},
        {name: 'Pipelines', href: '/pipelines'},
        {name: 'Entity View', href: '/entities'},
        {name: 'Admin', href: '/admin'},
    ];
</script>

<nav class="bg-gray-50 border-b border-gray-200">
    <div class="max-w-screen-xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between h-16">
            <!-- Left: Home Button -->
            <div class="flex-shrink-0">
                <a href="/" class="text-xl font-bold text-gray-800 hover:text-gray-600">
                    Home
                </a>
            </div>

            <!-- Center: Links (hidden on small screens) -->
            <div class="hidden md:flex items-center space-x-4 flex-grow mx-5">
                {#each links as link}
                    <a
                            href={link.href}
                            class="text-gray-800 hover:text-gray-600 transition duration-150"
                    >
                        {link.name}
                    </a>
                {/each}
            </div>

            <!-- Right: User Icon + Burger Menu -->
            <div class="flex items-center space-x-6">
                <button
                        on:click={() => { showMenu = !showMenu;}}
                        class="md:hidden text-gray-800 hover:text-gray-600"
                >
                    <!-- Burger Icon -->
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor"
                         class="w-6 h-6">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                              d="M4 6h16M4 12h16m-7 6h7"/>
                    </svg>
                </button>

                <!-- User Icon -->
                <a href="/profile" class="text-gray-800 hover:text-gray-600" aria-label="Link to user profile">
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5"
                         stroke="currentColor" class="size-6">
                        <path stroke-linecap="round" stroke-linejoin="round"
                              d="M15.75 6a3.75 3.75 0 1 1-7.5 0 3.75 3.75 0 0 1 7.5 0ZM4.501 20.118a7.5 7.5 0 0 1 14.998
                              0A17.933 17.933 0 0 1 12 21.75c-2.676 0-5.216-.584-7.499-1.632Z"/>
                    </svg>
                </a>
            </div>
        </div>
    </div>

    <!-- Mobile Menu -->
    {#if showMenu}
        <div class="md:hidden bg-white border-t border-gray-200 px-4 py-2">
            <div class="space-y-2">
                <a
                        href="/"
                        class="block text-gray-800 hover:text-gray-600 transition duration-150"
                >
                    Home
                </a>
                {#each links as link}
                    <a
                            href={link.href}
                            class="block text-gray-800 hover:text-gray-600 transition duration-150"
                    >
                        {link.name}
                    </a>
                {/each}
            </div>
        </div>
    {/if}
</nav>