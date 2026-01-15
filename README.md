# TableConverter - SQL Server Query Result Converter

SQL Serverのクエリ結果をSQL INSERT ステートメント（Table Values Constructor形式）に変換するコンソールアプリケーションです。

## 実装済み機能

### 1. Table Values Constructor生成
- SELECT クエリの実行結果をSQL INSERT ステートメントに自動変換
- 最大500行ごとの自動チャンク分割対応
- 空の結果セットへのコメント処理

### 2. 包括的なデータ型サポート
- **NULL型:** NULL処理
- **文字列型:** Unicode対応（N'値'）、SQL インジェクション対策
- **日時型:** DateTime（ISO 8601）、DateTimeOffset（タイムゾーン付き）、TimeSpan
- **数値型:** Decimal（28-29桁精度）、Double（G17形式）、Float（G9形式）、各整数型
- **その他型:** Guid、Boolean、Byte配列（16進数形式）
- **特殊値:** NaN/Infinity自動NULL変換

### 3. ロケール不変処理
- CultureInfo.InvariantCultureを使用
- 任意のシステムロケールで一貫した出力

### 4. セキュリティ機能
- シングルクォート自動エスケープ（'' に変換）
- SQL インジェクション対策
- Byte配列の安全な16進数変換

### 5. 柔軟な列名処理
- スペースと特殊文字対応
- 角括弧修飾

### 6. 包括的なユニットテスト
- 70以上のxUnitテスト
- エッジケースカバレッジ
- ロケール互換性検証

## 使用方法

### コマンド形式
```
TableConverter <server> <database> <query>
```

### 使用例
```bash
TableConverter .\\SQLEXPRESS MyDB "SELECT * FROM Users WHERE Active = 1"
```

### エラーコード
- `1`: 引数不足
- `2`: データベースエラー
- `3`: クエリが結果を返さない
- `99`: その他のエラー

## 技術仕様

### 要件
- .NET 8.0以上

### 依存ライブラリ
- **Dapper** 2.1.15: ORM・SQL実行
- **System.Data.SqlClient** 4.8.6: SQL Server接続

### テスト
- **xUnit** 2.6.5: ユニットテストフレームワーク

## 今後の拡張予定

- [ ] Generate CSV.
- [ ] Generate JSON.
- [ ] Generate Excel.
