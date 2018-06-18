<template v-for="option in options">
    <input type="radio" v-bind:value="option" v-model="selectedOption">
    <label>{{option.name}}</label>
</template>

<script>
export default {
  name: "filter-textbox",
  props: {
    options: Array
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
    clearData() {
      this.value.text = "";
    }
  }
};
</script>