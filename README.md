# TableConverter - SQL Server Query Result Converter

SQL Serverのクエリ結果をSQL INSERT ステートメント（Table Values Constructor形式）に変換するコンソールアプリケーションです。

## 実装済み機能

### 1. 複数の出力形式対応
- **SQL**: Table Values Constructor形式のINSERT ステートメント（デフォルト）
- **CSV**: RFC 4180準拠のCSV形式ファイル
- **JSON**: オブジェクト配列形式のJSON（見やすく整形）
- **Excel**: .xlsx形式のExcelファイル

### 2. Table Values Constructor生成（SQL形式）
- SELECT クエリの実行結果をSQL INSERT ステートメントに自動変換
- 最大500行ごとの自動チャンク分割対応
- 空の結果セットへのコメント処理

### 3. 包括的なデータ型サポート
- **NULL型:** NULL処理
- **文字列型:** Unicode対応（N'値'）、SQL インジェクション対策
- **日時型:** DateTime（ISO 8601）、DateTimeOffset（タイムゾーン付き）、TimeSpan
- **数値型:** Decimal（28-29桁精度）、Double（G17形式）、Float（G9形式）、各整数型
- **その他型:** Guid、Boolean、Byte配列（16進数形式）
- **特殊値:** NaN/Infinity自動NULL変換

### 4. ロケール不変処理
- CultureInfo.InvariantCultureを使用
- 任意のシステムロケールで一貫した出力

### 5. セキュリティ機能
- シングルクォート自動エスケープ（'' に変換）
- SQL インジェクション対策
- Byte配列の安全な16進数変換

### 6. 柔軟な列名処理
- スペースと特殊文字対応
- 角括弧修飾

### 7. 柔軟な出力オプション
- コンソール出力またはファイル出力を選択可能
- `--format` で出力形式を指定
- `--output` でファイルパスを指定

### 8. 包括的なユニットテスト
- 90以上のxUnitテスト（フォーマッターテスト含む）
- エッジケースカバレッジ
- ロケール互換性検証

## 使用方法

### コマンド形式（後方互換）
```bash
TableConverter <server> <database> <query>
```

### コマンド形式（新しい形式）
```bash
TableConverter <server> <database> <query> [--format <format>] [--output <file>]
```

### 使用例

#### SQL形式（デフォルト、コンソール出力）
```bash
TableConverter .\\SQLEXPRESS MyDB "SELECT * FROM Users WHERE Active = 1"
```

#### CSV形式（ファイル出力）
```bash
TableConverter .\\SQLEXPRESS MyDB "SELECT * FROM Users" --format CSV --output users.csv
```

#### JSON形式（ファイル出力）
```bash
TableConverter .\\SQLEXPRESS MyDB "SELECT * FROM Users" --format JSON --output users.json
```

#### Excel形式（ファイル出力が必須）
```bash
TableConverter .\\SQLEXPRESS MyDB "SELECT * FROM Users" --format Excel --output users.xlsx
```

### オプション
- `--format, -f`: 出力形式 (SQL/CSV/JSON/Excel、デフォルト: SQL)
- `--output, -o`: 出力ファイルパス（指定なしの場合はコンソール出力）

### エラーコード
- `1`: 引数不足またはパース失敗
- `2`: データベースエラー
- `3`: クエリが結果を返さない
- `4`: Excel形式で--output未指定
- `5`: ファイルI/Oエラー
- `6`: ファイルアクセス拒否
- `7`: 無効な引数
- `99`: その他のエラー

## 技術仕様

### 要件
- .NET 8.0以上

### 依存ライブラリ
- **Dapper** 2.1.15: ORM・SQL実行
- **System.Data.SqlClient** 4.8.6: SQL Server接続
- **CsvHelper** 33.0.1: CSV形式生成（RFC 4180準拠）
- **ClosedXML** 0.102.3: Excel形式生成
- **System.Text.Json**: JSON形式生成（.NET 8.0に含まれる）

### テスト
- **xUnit** 2.6.5: ユニットテストフレームワーク

## 今後の拡張予定

- [ ] XML形式対応
- [ ] Parquet形式対応
- [ ] ストリーミング出力対応（大規模データセット用）
- [ ] 接続文字列設定ファイル対応
- [ ] クエリパラメータ化対応
