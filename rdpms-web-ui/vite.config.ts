import tailwindcss from '@tailwindcss/vite';
import { sveltekit } from '@sveltejs/kit/vite';
import { defineConfig } from 'vite';
import { viteStaticCopy } from 'vite-plugin-static-copy';

export default defineConfig(({ mode }) => {
	const plugins = [
		tailwindcss(),
		sveltekit(),
	]

	console.log(`mode: ${mode}`);

	if (mode in ['dev', 'devel', 'development']) {
		return {
			plugins: plugins
		};
	}

	plugins.push(
		viteStaticCopy({
			targets: [
				{
					src: './config/debug/config.json', // File to copy
					dest: '.' // Destination inside `build/`
				}
			]
		})
	)

	return {
		plugins: plugins
	};
});
