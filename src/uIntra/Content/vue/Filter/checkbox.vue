<template>
    <div class="check">
        <label class="check__label">
            <input type="checkbox" class="check__box" name="filterState.showPinned" :checked="checked">
            <span class="check__fake-input icon-check"></span>
            <span class="check__fake-label">{{label}}</span>
        </label>
    </div>
</template>

<script>
export default {
    name: 'filter-checkbox',
    props:{
        label: String,
        checked:{
            type: Boolean,
            default: false
        }
    },
    methods:{

    }
}
</script>
