<template>
    <input class="txt txt--small" type="text" :placeholder='value.placeholder' v-model="val">
</template>

<script>
export default {
  name: "filter-textbox",
  props: {
    data: Object,
  },
  computed: {
    val: {
      get: function() {
        return this.value.text;
      },
      set: function(v) {
        this.value.text = v;
      }
    }
  },
  methods: {

  }
};
</script>
