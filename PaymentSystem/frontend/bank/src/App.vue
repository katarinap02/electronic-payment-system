<template>
  <div id="app">
    <h1 class="centered-title">Bank Payment System</h1>
    <router-view></router-view>
  </div>
</template>

<script>
export default {
  name: 'App'
}
</script>

<style scoped>
.centered-title {
  text-align: center;
  margin: 20px 0;
}
</style>