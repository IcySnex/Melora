<script setup>
	import { SourceIcon } from "vuepress-shared/client";
    import { ref, onMounted } from 'vue';  // Import ref and onMounted
	
	const props = defineProps({
        manifestUrls: {
	    type: Array,
	    required: true
	  }
	})

    const manifests = ref([]);
    const loading = ref(false);
    const sentinel = ref(null);

    const fetchManifests = async (start = 0, limit = 10) => {
        const urlsToFetch = props.manifestUrls.slice(start, start + limit);

        const fetchedManifests = await Promise.all(
            urlsToFetch.map(async (url) => {
                const response = await fetch(url);
                return await response.json();
            })
        );

        manifests.value.push(...fetchedManifests);
    };

    const loadMore = () => {
        if (!loading.value) {
            loading.value = true;
            fetchManifests(manifests.value.length, 10).finally(() => {
                loading.value = false;
            });
        }
    };

    onMounted(() => {
        const observer = new IntersectionObserver((entries) => {
            if (entries[0].isIntersecting) {
                loadMore();
            }
        }, {
            rootMargin: '100px', 
        });

        if (sentinel.value) {
            observer.observe(sentinel.value);
        }
    });

	
	const download = (url) => {
	    const isConfirmed = window.confirm("Are you sure you want to download this plugin?")
	    if (isConfirmed) {
	        window.location.href = url;
	    }
	}
    
	const ignoreClick = (event) => {
	    event.stopPropagation()
	}
	
	const formatDate = (dateString) => {
        const date = new Date(dateString)
	    return new Intl.DateTimeFormat('en-GB', { day: '2-digit', month: 'short', year: 'numeric' }).format(date)
	}
	
</script>

<template>
	<div class="card-container">
		<div
			v-for="(manifest, index) in manifests"
			:key="index"
			class="card"
			@click="download(manifest.DownloadUrl)">

			<h3 class="card-name">
				{{ manifest.Name }}

				<a @click="ignoreClick" class="card-source-icon-container" :href="manifest.SourceUrl" target="_blank" rel="noopener noreferrer">
					<SourceIcon class="card-source-icon" />
				</a>
			</h3>
			<p class="card-info">
				{{ manifest.Author }} - {{ formatDate(manifest.LastUpdatedAt) }} - API: {{  manifest.ApiVersion }}
			</p>

			<p class="card-description">
				{{ manifest.Description }}
			</p>
		</div>

        <div ref="sentinel" class="sentinel"></div>
	</div>
</template>

<style lang="scss">
    .sentinel {
        height: 1px;
        width: 100%;
    }

    .card-container {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
        gap: 1rem;
        padding: 20px;
    }
  
    .card {
        padding: 1rem;
        border-radius: 0.5rem;
        border: 2px solid var(--vp-c-accent);
        box-shadow: 0px 4px 8px var(--card-shadow);
        cursor: pointer;

        transition:
            box-shadow 100ms linear;
  
        &:hover {
            background-color: var(--vp-c-bg-alt);
            box-shadow: 0 4px 32px var(--vp-c-accent-text);
        }
    }

    .card-source-icon-container {
        float: right;
        display: flex;
        align-items: center;
        
        transition:
            transform 100ms linear;
        
        &:hover {
            transform: scale(1.1);
        }
    }

    .card-source-icon {
        float: right;
        width: 1.5rem;
        height: 1.5rem;
        color: var(--text-color);
    }
    
    .card-name {
        margin: 0;
        color: var(--text-color-light);
        font-weight: bold;
        font-size: 1.3rem;
    
        @media (max-width: hope-config.$pad) {
            font-size: 1.2rem;
        }
    }
    
    .card-info {
        margin: 0;
        margin-bottom: 10px;
        color: var(--text-color-lightest);
        line-height: 1.4;
        font-size: 0.9rem;
        overflow: auto;
    
        @media (max-width: hope-config.$pad) {
            font-size: 0.8rem;
        }
    }
    
    .card-description {
        margin: 0;
        margin-bottom: 4px;
        color: var(--text-color-lighter);
        line-height: 1.4;
        font-size: 1rem;
        max-height: 4rem;
        overflow: auto;
    
        @media (max-width: hope-config.$pad) {
            font-size: 0.9rem;
        }
    }
</style>