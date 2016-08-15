let canvas = document.createElement('canvas');
console.log(canvas);
console.log('Canvas created');
try {
    let context = canvas.getContext('webgl-holographic');
    console.log('Got context:');
    console.log(context);
}
catch (e) {
    console.log(e.message);
    console.log(e.description);
}
