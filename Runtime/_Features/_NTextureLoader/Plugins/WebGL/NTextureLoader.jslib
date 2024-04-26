mergeInto(LibraryManager.library,
    {
        process: function (id, base64Data, ext, maxWidth, maxHeight) {
            var extStr = UTF8ToString(ext);
            var base64Str = UTF8ToString(base64Data);
            var img = new Image();
            img.src = "data:image/" + extStr + ";base64," + base64Str;

            img.onload = function () {
                var ratioW = img.width / maxWidth;
                var ratioH = img.height / maxHeight;

                var ratio = ratioW > ratioH ? ratioW : ratioH;
                ratio = ratio > 1 ? ratio : 1;

                try {
                    var canvas = document.createElement('canvas');
                    canvas.width = img.width / ratio;
                    canvas.height = img.height / ratio;

                    var ctx = canvas.getContext('2d');
                    ctx.translate(0, canvas.height);
                    ctx.scale(1, -1);
                    ctx.drawImage(img, 0, 0, canvas.width, canvas.height);

                    var imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
                    var bytes = new Uint8Array(imageData.data);
                    var dataPointer = _malloc(bytes.byteLength);
                    var dataHeap = new Uint8Array(HEAPU8.buffer, dataPointer, bytes.byteLength);
                    dataHeap.set(bytes);

                    var result = JSON.stringify({
                        id: id,
                        resultPointer: dataPointer,
                        length: bytes.byteLength,
                        originWidth: img.width,
                        originHeight: img.height,
                        outWidth: imageData.width,
                        outHeight: imageData.height,
                    });
                    SendMessage("NTextureProcessWebGLCallback", "onProcessCompleteCallback", result);
                    ctx.clearRect(0, 0, canvas.width, canvas.height);
                }
                catch (error) {
                    var result = JSON.stringify({
                        id: id,
                        error: error.toString(),
                    });
                    SendMessage("NTextureProcessWebGLCallback", "onProcessCompleteCallback", result);
                }
            }
        },
        processWithSampleSize: function (id, base64Data, ext, inSampleSize) {
            var extStr = UTF8ToString(ext);
            var base64Str = UTF8ToString(base64Data);
            var img = new Image();
            img.src = "data:image/" + extStr + ";base64," + base64Str;

            img.onload = function () {
                var ratio = inSampleSize;

                try {
                    var canvas = document.createElement('canvas');
                    canvas.width = img.width / ratio;
                    canvas.height = img.height / ratio;

                    var ctx = canvas.getContext('2d');
                    ctx.translate(0, canvas.height);
                    ctx.scale(1, -1);
                    ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
                    
                    var imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
                    var bytes = new Uint8Array(imageData.data);
                    var dataPointer = _malloc(bytes.byteLength);
                    var dataHeap = new Uint8Array(HEAPU8.buffer, dataPointer, bytes.byteLength);
                    dataHeap.set(bytes);

                    var result = JSON.stringify({
                        id: id,
                        resultPointer: dataPointer,
                        length: bytes.byteLength,
                        originWidth: img.width,
                        originHeight: img.height,
                        outWidth: imageData.width,
                        outHeight: imageData.height,
                    });
                    SendMessage("NTextureProcessWebGLCallback", "onProcessCompleteCallback", result);
                    ctx.clearRect(0, 0, canvas.width, canvas.height);
                }
                catch (error) {
                    var result = JSON.stringify({
                        id: id,
                        error: error.toString(),
                    });
                    SendMessage("NTextureProcessWebGLCallback", "onProcessCompleteCallback", result);
                }
            }
        },
        processWithUrlAndSampleSize: function (id, url, inSampleSize) {
            var urlStr = UTF8ToString(url);
            var img = new Image();
            img.decoding = "async";
            img.setAttribute('crossOrigin', '');
            img.src = urlStr;

            img.onload = function () {
                var ratio = inSampleSize;

                try {
                    var canvas = document.createElement('canvas');
                    canvas.width = img.width / ratio;
                    canvas.height = img.height / ratio;

                    var ctx = canvas.getContext('2d');
                    ctx.translate(0, canvas.height);
                    ctx.scale(1, -1);
                    ctx.drawImage(img, 0, 0, canvas.width, canvas.height);

                    var imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
                    var bytes = new Uint8Array(imageData.data);
                    var dataPointer = _malloc(bytes.byteLength);
                    var dataHeap = new Uint8Array(HEAPU8.buffer, dataPointer, bytes.byteLength);
                    dataHeap.set(bytes);
                    
                    var result = JSON.stringify({
                        id: id,
                        resultPointer: dataPointer,
                        length: bytes.byteLength,
                        originWidth: img.width,
                        originHeight: img.height,
                        outWidth: imageData.width,
                        outHeight: imageData.height,
                    });
                    SendMessage("NTextureProcessWebGLCallback", "onProcessCompleteCallback", result);
                    ctx.clearRect(0, 0, canvas.width, canvas.height);
                }
                catch (error) {
                    var result = JSON.stringify({
                        id: id,
                        error: error.toString(),
                    });
                    SendMessage("NTextureProcessWebGLCallback", "onProcessCompleteCallback", result);
                }
            }
        },
        processWithUrl: function (id, url, maxWidth, maxHeight) {
            var urlStr = UTF8ToString(url);
            var img = new Image();
            img.decoding = "async";
            img.setAttribute('crossOrigin', '');
            img.src = urlStr;

            img.onload = function () {
                var ratioW = img.width / maxWidth;
                var ratioH = img.height / maxHeight;

                var ratio = ratioW > ratioH ? ratioW : ratioH;
                ratio = ratio > 1 ? ratio : 1;

                try {
                    var canvas = document.createElement('canvas');
                    canvas.width = img.width / ratio;
                    canvas.height = img.height / ratio;

                    var ctx = canvas.getContext('2d');
                    ctx.translate(0, canvas.height);
                    ctx.scale(1, -1);
                    ctx.drawImage(img, 0, 0, canvas.width, canvas.height);

                    var imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
                    var bytes = new Uint8Array(imageData.data);
                    var dataPointer = _malloc(bytes.byteLength);
                    var dataHeap = new Uint8Array(HEAPU8.buffer, dataPointer, bytes.byteLength);
                    dataHeap.set(bytes);

                    var result = JSON.stringify({
                        id: id,
                        resultPointer: dataPointer,
                        length: bytes.byteLength,
                        originWidth: img.width,
                        originHeight: img.height,
                        outWidth: imageData.width,
                        outHeight: imageData.height,
                    });
                    SendMessage("NTextureProcessWebGLCallback", "onProcessCompleteCallback", result);
                    ctx.clearRect(0, 0, canvas.width, canvas.height);
                }
                catch (error) {
                    var result = JSON.stringify({
                        id: id,
                        error: error.toString(),
                    });
                    SendMessage("NTextureProcessWebGLCallback", "onProcessCompleteCallback", result);
                }
            }
        },
        loadTextureAtNative: function (id, url, inSampleSize, tex) {
            var urlStr = UTF8ToString(url);
            var img = new Image();
            img.decoding = "async";
            img.setAttribute('crossOrigin', '');
            img.src = urlStr;

            img.onload = function () {

                try{
                    var outWidth = img.width / inSampleSize;
                    var outHeight = img.height / inSampleSize;

                    var canvas = document.createElement('canvas');
                    canvas.width = outWidth;
                    canvas.height = outHeight;
                    var ctx = canvas.getContext('2d');
                    ctx.translate(0, canvas.height);
                    ctx.scale(1, -1);
                    ctx.drawImage(img, 0, 0, canvas.width, canvas.height);

                    var imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
                
                    GLctx.deleteTexture(GL.textures[tex]);
                    var t = GLctx.createTexture();
                    t.name = tex;
                    GL.textures[tex] = t;
                
                    GLctx.bindTexture(GLctx.TEXTURE_2D, GL.textures[tex]);
                    GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_MIN_FILTER, GLctx.LINEAR);
                    GLctx.texImage2D(GLctx.TEXTURE_2D, 0, GLctx.RGBA, outWidth, outHeight, 0, GLctx.RGBA, GLctx.UNSIGNED_BYTE, imageData);
                    GLctx.bindTexture(GLctx.TEXTURE_2D, null);
                
                    var result = JSON.stringify({
                        id: id,
                        originWidth: img.width,
                        originHeight: img.height,
                        outWidth: outWidth,
                        outHeight: outHeight,
                        texPtr: tex,
                    });
                    SendMessage("NTextureProcessWebGLCallback", "loadTextureAtNativeCompleteCallback", result);
                    ctx.clearRect(0, 0, canvas.width, canvas.height);
                }
                catch(error){
                    var result = JSON.stringify({
                        id: id,
                        error: error.toString(),
                    });
                    SendMessage("NTextureProcessWebGLCallback", "loadTextureAtNativeCompleteCallback", result);
                }
            }
        }
    });