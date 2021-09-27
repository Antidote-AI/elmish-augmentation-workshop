const path = require("path")
const HtmlWebpackPlugin = require('html-webpack-plugin')
const MiniCssExtractPlugin = require('mini-css-extract-plugin')

function resolve(filePath) {
    return path.join(__dirname, filePath)
}

module.exports = (_env, options) => {

    var isDevelopment = options.mode === "development";

    return {
        entry: './fableBuild/App.js',
        mode: isDevelopment ? "development" : "production",
        output: {
            path: resolve('./temp'),
            filename: isDevelopment ? '[name].js' : '[name].[fullhash].js',
        },
        devtool: isDevelopment ? 'eval-source-map' : false,
        optimization: {
            // Split the code coming from npm packages into a different file.
            // 3rd party dependencies change less often, let the browser cache them.
            splitChunks: {
                cacheGroups: {
                    commons: {
                        test: /node_modules/,
                        name: "vendors",
                        chunks: "all"
                    }
                }
            },
        },
        plugins:
            [
                new HtmlWebpackPlugin({
                    filename: "./index.html",
                    template: "./sources/index.html"
                }),
                new MiniCssExtractPlugin()
            ].filter(Boolean),
        devServer: {
            port: 8080,
            hot: true,
            static: {
                publicPath: "/",
            }
        },
        module: {
            rules: [
                {
                    test: /\.(sass|scss|css)$/,
                    use: [
                        MiniCssExtractPlugin.loader,
                        'css-loader',
                        {
                            loader: 'sass-loader',
                            options: {
                                sassOptions: {
                                    quietDeps: true // Disable slash div warning coming from dependencies like FontAwesome
                                }
                            }
                        }
                    ],
                },
                {
                    test: /\.(png|jpg|jpeg|gif|svg|woff|woff2|ttf|eot)$/,
                    use: ["file-loader"]
                }
            ]
        }
    }
}
